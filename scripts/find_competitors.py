from github import Github
import json

# First create a Github instance using an access token
g = Github("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")

# Get the repository you want to list the forks of
repo = g.get_repo("SebLague/Chess-Challenge")

# Get all the forks of the repository
forks = repo.get_forks()

# Filter the forks with over 30 commits
data_forks = []
for fork in forks:
    data_forks.append({
        "name": fork.full_name,
        "commits": fork.get_commits().totalCount
    })

with open("data.json", "w") as file:
    file.write(json.dumps(data_forks))