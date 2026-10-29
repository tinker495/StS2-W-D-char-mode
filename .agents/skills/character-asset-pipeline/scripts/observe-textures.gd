extends SceneTree

# Exports complete native textures and reports measurements. A comparison result
# is data for visual review, not a product acceptance verdict.
func _initialize() -> void:
    call_deferred("observe")

func fail(message: String) -> void:
    push_error(message)
    quit(1)

func observe() -> void:
    var args := OS.get_cmdline_user_args()
    if args.size() != 1:
        fail("Expected one observation request JSON path")
        return
    var parsed: Variant = JSON.parse_string(FileAccess.get_file_as_string(args[0]))
    if not parsed is Dictionary:
        fail("Cannot read observation request")
        return
    var request: Dictionary = parsed
    if not ProjectSettings.load_resource_pack(request["pack"]):
        fail("Cannot load pack: " + str(request["pack"]))
        return
    var comparison: Image = null
    var source_path: String = request.get("source", "")
    if not source_path.is_empty():
        comparison = Image.load_from_file(source_path)
        if comparison == null or comparison.is_empty():
            fail("Cannot read comparison PNG: " + source_path)
            return
        comparison.convert(Image.FORMAT_RGBA8)
    var images: Array = []
    for selected: Dictionary in request["resources"]:
        var resource_path: String = selected["resource"]
        var resource: Resource = load(resource_path)
        if not resource is Texture2D:
            fail("Resource is not a loadable Texture2D: " + resource_path)
            return
        var picture: Image = resource.get_image()
        if picture == null or picture.is_empty():
            fail("Cannot read texture pixels: " + resource_path)
            return
        if picture.is_compressed() and picture.decompress() != OK:
            fail("Cannot decompress texture pixels: " + resource_path)
            return
        picture.convert(Image.FORMAT_RGBA8)
        var output_path: String = str(request["output_directory"]).path_join(selected["file"])
        if picture.save_png(output_path) != OK:
            fail("Cannot save texture PNG: " + output_path)
            return
        var item := {
            "resource": resource_path,
            "file": selected["file"],
            "width": picture.get_width(),
            "height": picture.get_height(),
            "fully_opaque": picture.detect_alpha() == Image.ALPHA_NONE
        }
        if comparison != null:
            item["source_width"] = comparison.get_width()
            item["source_height"] = comparison.get_height()
            item["pixels_equal_source"] = picture.get_size() == comparison.get_size() and picture.get_data() == comparison.get_data()
        images.append(item)
    var manifest := {
        "command": request["command"],
        "pack": request["pack"],
        "pack_size_bytes": request["pack_size_bytes"],
        "pack_modified_utc": request["pack_modified_utc"],
        "game_version": request["game_version"],
        "godot_version": Engine.get_version_info()["string"],
        "observed_utc": Time.get_datetime_string_from_system(true),
        "source": source_path,
        "images": images,
        "complete": true
    }
    var manifest_path: String = str(request["output_directory"]).path_join("manifest.json")
    var file := FileAccess.open(manifest_path, FileAccess.WRITE)
    if file == null:
        fail("Cannot write manifest: " + manifest_path)
        return
    file.store_string(JSON.stringify(manifest, "  "))
    file.close()
    print("TEXTURE_OBSERVATIONS_COMPLETE count=", images.size())
    quit(0)
