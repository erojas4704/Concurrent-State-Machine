if [ -f "version" ]; then
  version=$(cat version)
else
  echo "0.0.1" > version
  version="0.0.1"
fi

read -p "Enter the version number (leave blank to leave it automatic): " user_version

if [ -n "$user_version" ]; then
  version="$user_version"
else
  version=$(awk '{split($0,a,"."); a[3]++; print a[1]"."a[2]"."a[3]}' version)
fi

echo $version > version
echo $version

target_dir=C:/Unity/Libraries/CSM/$version/

mkdir -p "$target_dir"

sed -i 's/\("version": "\)[^"]*/\1'$version'/' package.json

cp -r ./Editor/CSM "$target_dir/Editor"
cp -r ./Scripts/CSM "$target_dir/Scripts"
cp package.json "$target_dir"


