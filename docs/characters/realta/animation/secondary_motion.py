"""Deterministic damped angular springs baked into the AnimationPlayer, never runtime physics.

Parent acceleration excites the hat, coat hem and ribbon. Loops are warmed for twelve cycles;
actions start at the idle equilibrium and return to it with a smooth terminal envelope.
"""
import math
import numpy as np

def bake(data, world, weapon_points, weapon_turn):
    rest = data['rest']
    rest['coat_hem'] = [242, 402, 'coat']
    report = {}
    idle_targets = {}
    for name, clip in data['clips'].items():
        samples = clip['samples']; times = np.array([s['time'] for s in samples]); L=clip['length']
        # Uniform primary samples carry their own times (240 Hz attack, 60 Hz other clips).
        for s in samples:
            s['joints']['coat']['rotation'] = 0.0
            s['joints'].setdefault('coat_hem', {'position':[0,102],'rotation':0.0})
        components = [('hat_tip','head',3.0,.72,.22,-1/180,.12),
                      ('coat_hem','torso',2.4,.8,.1,1/260,.10),
                      ('ribbon','weapon',3.8,.68,.16,1/220,.22)]
        tracks = {}; peaks={}
        for joint,parent,hz,damping,inertia,linear_gain,limit in components:
            poses = [world(s['joints'],parent) for s in samples]
            if name=='die' and parent=='weapon': poses=data['death_prop_world']
            angle = np.unwrap([p[1] for p in poses]); x = np.array([p[0][0] for p in poses])
            # Fine integration avoids numerical ringing from the short anticipation.
            dt=1/240; count=round(L/dt); fine=np.arange(count+1)*dt
            a=np.interp(fine,times,angle); px=np.interp(fine,times,x)
            if clip['loop']:
                def deriv(v): return (np.roll(v,-1)-np.roll(v,1))/(2*dt)
                av=deriv(a[:-1]); xv=deriv(px[:-1]); acc=deriv(av); xacc=deriv(xv)
                forcing=-inertia*acc+linear_gain*xacc
                # Very small cloth drift, with the same exact period as the parent clip.
                forcing += .35*np.sin(math.tau*fine[:-1]/L-.5)
            else:
                forcing=-inertia*np.gradient(np.gradient(a,dt),dt)+linear_gain*np.gradient(np.gradient(px,dt),dt)
            omega=math.tau*hz; q=0.0; v=0.0; out=[]
            cycles=12 if clip['loop'] else 1
            for cycle in range(cycles):
                out=[]
                for k in range(count):
                    # Smooth saturation limits physically impossible hinge folds, not velocities.
                    drive=omega*omega*limit*math.tanh(float(forcing[k])/(omega*omega*limit))
                    v += (drive-2*damping*omega*v-omega*omega*q)*dt
                    q += v*dt; out.append(q)
            vals=np.interp(times,fine[:count],out)
            if clip['loop']:
                vals[-1]=vals[0]
                if name=='idle_loop': idle_targets[joint]=float(vals[0])
            else:
                equilibrium=idle_targets.get(joint,0.0)
                for k,t in enumerate(times):
                    # No residual spring/reset pop on next=idle. Death is hidden before this tail.
                    u=min(1.0,max(0.0,(L-t)/min(.3,L*.3)))
                    envelope=u*u*(3-2*u)
                    vals[k]=equilibrium+vals[k]*envelope
            peaks[joint]=round(float(np.max(np.abs(vals))),5)
            if joint=='ribbon':
                if name in ('attack','die'):
                    # The knot follows the painted socket through its shaft-axis reflection,
                    # while the cloth itself is not mirrored. Held tails hang downward;
                    # released tails trail the falling socket across the brim, above the face.
                    ribbon_angles=[]; positions=[]; scales=[]
                    for k,(t,value) in enumerate(zip(times,vals)):
                        o,r=poses[k]
                        turn=weapon_turn(float(t)) if name=='attack' else 1.0
                        knot=weapon_points(np.array([[-13.0,-84.0]]),turn)[0]
                        target=math.pi/2+.22+max(-.20,min(.20,float(value)))
                        if name=='die':
                            rise=max(0.0,min(1.0,(float(t)-.22)/.16))
                            fall=max(0.0,min(1.0,(float(t)-.50)/.20))
                            # Smooth inertial sweep before floor compression. The former
                            # gravity-only tail covered the eyes at .42..50 s once frontmost.
                            target+=1.7*(rise*rise*(3-2*rise))*(1-fall*fall*(3-2*fall))
                        local=target-r-math.atan2(60,-28)
                        size=1.0
                        if name=='die':
                            if t>=.8:local=math.pi/2+.22-r-math.atan2(60,-28)
                            knot_world_y=o[1]+math.sin(r)*knot[0]+math.cos(r)*knot[1]
                            # Preserve the complete ribbon painting, compressed as it meets floor.
                            size=min(1.0,max(.08,(526.0-knot_world_y)/105.0))
                        ribbon_angles.append([float(t),float(local)])
                        positions.append([float(t),list(knot)]);scales.append([float(t),[size,size]])
                    tracks['%Ribbon:rotation']=ribbon_angles
                    tracks['%Ribbon:position']=positions;tracks['%Ribbon:scale']=scales
                else:tracks['%Ribbon:rotation']=[[float(t),float(v)] for t,v in zip(times,vals)]
            else:
                for s,value in zip(samples,vals): s['joints'][joint]['rotation']=float(value)
        clip['secondary_tracks']=tracks
        report[name]={'warmup_cycles':12 if clip['loop'] else 0,'peak_rotation_rad':peaks,
                      'loop_endpoint_error':max(abs(samples[0]['joints'][j]['rotation']-samples[-1]['joints'][j]['rotation']) for j in ('hat_tip','coat_hem')) if clip['loop'] else None}
    return report
