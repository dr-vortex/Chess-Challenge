from github import Github
import json
import os

# First create a Github instance using an access token
g = Github("");
repo = g.get_repo("SebLague/Chess-Challenge")
forks = repo.get_forks()

data_forks = []
for fork in forks:
    try:
        data_forks.append({
            "name": fork.full_name,
            "commits": fork.get_commits().totalCount
        })
    except:
        pass

sorted_data = sorted(data_forks, key=lambda k: k['commits'], reverse=True)

out = ""

for entry in sorted_data:
    out += "https://github.com/" + entry["name"] + " " + str(entry["commits"]) + "\n"

with open("most_active.txt", "w") as file:
    file.write(out)