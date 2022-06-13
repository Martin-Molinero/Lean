#!/bin/bash

git config user.name "$(git log -n 1 --pretty=format:%an)"
git config user.email "$(git log -n 1 --pretty=format:%ae)"

for branch in $(git for-each-ref refs/heads/* | cut -d"$(printf '\t')" -f2 | cut -b12- | grep ^org-)
do
    echo "Rebasing branch $branch"
    git checkout $branch
    git merge master --commit --no-edit
    retVal=$?
    if [ $retVal -eq 0 ]; then
        echo "Pushing branch $branch"
        git push origin $branch
    else
        echo "Rebase failed branch $branch"
        git merge --abort
    fi
    git checkout master
    git clean -xqdf
done