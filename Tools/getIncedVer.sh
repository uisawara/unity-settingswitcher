latest_version=`git tag | grep $1 | sed 's/\./ /g' | sort -n -k 2 -k 3 -k 4 | sed 's/ /\./g' | tail -n 1`
list=(${latest_version//./ })
cnt=${list[2]}
let cnt++
echo ${list[0]}.${list[1]}.$cnt
