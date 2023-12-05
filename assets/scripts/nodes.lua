local files = list_files_in_dir("nodes/")

for _, file in ipairs(files) do
    local node = require(file)
    table.extend(data, { node })
end
