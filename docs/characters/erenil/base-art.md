# 에레닐의 전신 원화

## 외형

은빛 곡선 단검을 든 [전신 원화](base.png)를 에레닐의 얼굴과 복장, 무기 형태의 기준으로 삼는다. 옆으로 넘긴 긴 흑갈색 머리와 가는 땋은 머리, 은색 목걸이, 벽돌빛 튜닉의 마름모 자락, 크림색 바지와 끈을 교차해 감은 장화는 [첫 번째 참고 그림](refs/ref1.png)에서 가져왔다. 주황빛 눈과 양손의 마법은 [두 번째 참고 그림](refs/ref2.png)을 따르고, 옷깃과 소매는 첫 번째 그림처럼 여몄다.

왼손 위의 붉은 구체는 밝은 손바닥과 간격을 두어 작은 화면에서도 윤곽이 보인다. 오른손의 은빛 단검은 날을 따라 두 줄의 홈이 칼끝까지 휘어 내려간다. 손보호대는 손가락 바깥을 돌아 자루 끝과 연결되는 가느다란 은색 고리다. 머리와 튜닉의 그림자를 넓은 면으로 묶고 광택을 눌러, 얼굴과 두 손에 시선이 머물도록 했다.

[변형 1 · 황금 단검](base_variation.png)에서 오른손에 든 무기는 짧고 곧은 양날에 장검의 긴 손잡이와 넓은 십자형 손보호대가 달려 있다. 날부터 둥근 손잡이 끝까지 검 전체가 금으로 만든 듯 보인다.

[변형 2 · 빛의 장검](base_variation_2.png)에서는 손보호대에서 뻗은 백금빛 검날이 짧은 황금 날을 감싼다. 빛이 시작되는 부분은 반투명해 그 안으로 원래 단검의 윤곽이 보인다.

## Delivery and Integration

The [1024 × 1536 RGBA master](base.png) preserves the full composition and its transparent margins for later exports. Delivery preserves generated resolution and clears RGBA samples with alpha at or below 8. The master is a source painting. Current in-game bindings are listed in [asset status](erenil-assets.md).

The [1536 × 1536 RGBA variation](base_variation_2.png) extends the transparent canvas to fit the light blade at the same character scale. To align the two paintings, place the [gold dagger variation](base_variation.png) at pixel offset (408, 0) within the variation.
