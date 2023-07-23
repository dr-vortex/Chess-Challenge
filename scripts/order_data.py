import json
data = []

with open("data.json", "r") as file:
    data = json.loads(file.read())

sorted_data = sorted(data, key=lambda k: k['commits'], reverse=True)

out = ""

for entry in sorted_data:
    out += entry["name"] + " " + str(entry["commits"]) + "\n"

with open("most_active.txt", "w") as file:
    file.write(out)