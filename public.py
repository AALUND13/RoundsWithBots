import sys
import os
import shutil
import json

def write_new_manifest(manifest_path, version=None):
    if version:
        with open(manifest_path, "r") as file:
            manifest = json.load(file)
            manifest["version_number"] = version

        with open(manifest_path, "w") as file:
            json.dump(manifest, file, indent=4)

def package(public_dir, dll_build_dir, exported_dll_path, icon_path, manifest_path, readme_path, build_symbol_path):
    # If the directory doesn't exist, create it. If it does exist, delete it and create a new one
    if os.path.exists(public_dir):
        shutil.rmtree(public_dir)
    os.makedirs(public_dir)

    # Copy the files
    for file_path in [icon_path, manifest_path, readme_path, exported_dll_path]:
        shutil.copy(file_path, public_dir)

    if os.path.exists(build_symbol_path):
        shutil.copy(build_symbol_path, public_dir)

    # Zip the directory
    output_zip_path = os.path.join(dll_build_dir, f"{os.path.splitext(os.path.basename(exported_dll_path))[0]}.zip")
    shutil.make_archive(os.path.splitext(output_zip_path)[0], 'zip', public_dir)

    print(f"Build successfully archived at: {output_zip_path}")

def unpackage(zip_path, extract_to):
    if not os.path.exists(zip_path):
        print(f"Error: Zip file '{zip_path}' not found.")
        sys.exit(1)

    if not os.path.exists(extract_to):
        os.makedirs(extract_to)

    shutil.unpack_archive(zip_path, extract_to)
    print(f"Archive successfully unpackaged to: {extract_to}")

def main():
    if len(sys.argv) < 3:
        print("Usage: script.py <exportedDLLPath> <version>")
        sys.exit(1)

    exported_dll_path = sys.argv[1]
    version = sys.argv[2]

    project_dir = os.path.dirname(os.path.realpath(__file__))
    dll_build_dir = os.path.dirname(os.path.realpath(exported_dll_path))

    icon_path = os.path.join(project_dir, "icon.png")
    manifest_path = os.path.join(project_dir, "manifest.json")
    readme_path = os.path.join(project_dir, "README.md")

    public_dir = os.path.join(dll_build_dir, "public")

    # Check if the files exist
    missing_files = []
    for path, name in [(icon_path, "icon.png"), (manifest_path, "manifest.json"), (readme_path, "README.md"), (exported_dll_path, "DLL")]:
        if not os.path.exists(path):
            missing_files.append(name)
    
    if missing_files:
        print(f"Error: Missing required files: {', '.join(missing_files)}")
        sys.exit(1)

    assembly_build_name = os.path.splitext(os.path.basename(exported_dll_path))[0]
    build_symbol_path = os.path.join(dll_build_dir, f"{assembly_build_name}.pdb")

    # Update the version in the manifest
    write_new_manifest(manifest_path, version)

    # Package the files
    package(public_dir, dll_build_dir, exported_dll_path, icon_path, manifest_path, readme_path, build_symbol_path)

if __name__ == "__main__":
    main()
