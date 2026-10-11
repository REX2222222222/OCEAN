#!/usr/bin/env bash
set -euo pipefail

root="$(pwd)"
project="$root/build/iOS/Unity-iPhone.xcodeproj"
derived="$root/build/iOSDerived"
output="$root/build/ipa"
test -d "$project"
mkdir -p "$output/Payload"

xcodebuild \
  -project "$project" \
  -scheme Unity-iPhone \
  -configuration Release \
  -sdk iphoneos \
  -arch arm64 \
  -derivedDataPath "$derived" \
  CODE_SIGNING_ALLOWED=NO \
  CODE_SIGNING_REQUIRED=NO \
  CODE_SIGN_IDENTITY= \
  build 2>&1 | tee "$root/build/xcodebuild.log"

apps=()
while IFS= read -r -d '' app; do
  apps+=("$app")
done < <(find "$derived/Build/Products/Release-iphoneos" -maxdepth 1 -type d -name '*.app' -print0)
if [[ "${#apps[@]}" -ne 1 ]]; then
  echo "Expected exactly one iPad app; found ${#apps[@]}." >&2
  exit 1
fi
app="${apps[0]}"
executable="$(/usr/libexec/PlistBuddy -c 'Print CFBundleExecutable' "$app/Info.plist")"
lipo "$app/$executable" -verify_arch arm64
ditto "$app" "$output/Payload/$(basename "$app")"
ditto -c -k --keepParent "$output/Payload" "$output/MarineEncyclopedia-unsigned.ipa"
unzip -tq "$output/MarineEncyclopedia-unsigned.ipa"
(cd "$output" && shasum -a 256 MarineEncyclopedia-unsigned.ipa > SHA256SUMS.txt)
echo "IPA created. Sign it locally before installing on your iPad."
