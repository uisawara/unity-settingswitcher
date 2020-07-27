latest_version_list=`git tag | grep $1`

# If first version, return default-first-version
is_first_version=`echo -n $latest_version_list | wc -c`
if [ $is_first_version = 0 ]; then
  echo "$1/v0.0.1"
  exit 0
fi

# return incremented patch version
latest_version=`echo $latest_version_list | sed 's/\./ /g' | sort -n -k 2 -k 3 -k 4 | sed 's/ /\./g' | tail -n 1`
list=(${latest_version//./ })
cnt=${list[2]}
let cnt++
echo ${list[0]}.${list[1]}.$cnt
