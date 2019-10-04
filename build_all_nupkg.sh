#!/bin/sh

printf '#####################################################################################\n'
printf 'This script will builds a nuget package for each CSharp project which has a *.nuspec.\n'
printf '#####################################################################################\n\n'

read -p "DO NOT FORGET TO UPDATE THE *.nuspec FILES! Press Enter to continue ..." 

pkgLoc='./bin.nuget'

printf 'Package location: %s' "$pkgLoc"
mkdir -p $pkgLoc
cd $pkgLoc

solutionDir='../'
i=0

for nuspecFile in "$solutionDir"/*/*.nuspec; do
        projectFile="${nuspecFile%.*}".csproj
        #printf '%d. %s\n\t- %s\n\n' "$(( ++i ))" "$project" "$readme"
        printf '%d. %s\n' "$(( ++i ))" "${projectFile%.*}"


        nuget pack $projectFile -Symbols -SymbolPackageFormat snupkg
done

cd ..