import sys, os, shutil, json

def writeNewManifest(manifestPath, version=None):
    if version:
        with open(manifestPath, "r") as file:
            manifest = json.load(file)
            manifest["version_number"] = version

        with open(manifestPath, "w") as file:
            json.dump(manifest, file, indent=4)

exportedDLLPath = sys.argv[1]

# If the version is not provided, default to 1.0.0
version = None if len(sys.argv) < 3 else sys.argv[2]

projectDir = os.path.dirname(os.path.realpath(__file__))
DLLBuildDir = os.path.dirname(os.path.realpath(exportedDLLPath))

iconPath = os.path.join(projectDir, "icon.png")
manifestPath = os.path.join(projectDir, "manifest.json")
readmePath = os.path.join(projectDir, "README.md")

publicDir = os.path.join(DLLBuildDir, "public")

# Check if the files exist
if not os.path.exists(iconPath):
    print("Error: icon.png not found, Please include an icon.png file")
    sys.exit(1)

if not os.path.exists(manifestPath):
    print("Error: manifest.json not found, Please include a manifest.json file")
    sys.exit(1)

if not os.path.exists(readmePath):
    print("Error: README.md not found, Please include a README.md file")
    sys.exit(1)

if not os.path.exists(exportedDLLPath):
    print("Error: DLL not found, Please build the project first")
    sys.exit(1)

# Update the version in the manifest
writeNewManifest(manifestPath, version)

# If the directory doesn't exist, create it
# If it does exist, delete it and create a new one
if not os.path.exists(publicDir):
    os.makedirs(publicDir)
else:
    shutil.rmtree(publicDir)
    os.makedirs(publicDir)

# Copy the files
shutil.copy(iconPath, publicDir)
shutil.copy(manifestPath, publicDir)
shutil.copy(readmePath, publicDir)
shutil.copy(exportedDLLPath, publicDir)

# Zip the directory
shutil.make_archive(os.path.splitext(exportedDLLPath)[0], 'zip', publicDir)