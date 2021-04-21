#pragma once

#include "integrated/ic_desc.hpp"
#include "projects/workspace_desc.hpp"
#include <vector>
#include <string>
#include <iostream>
#include <fstream>
#include "utils/json.hpp"
using json = nlohmann::json;

class Project {
public:
    std::vector<std::string> includedICs;
    std::string name;
    WorkspaceDesc workspace;

    Project(std::string name);

    std::vector<ICDesc> GetAllIncludedICs();
    void SaveWorkspace(std::vector<DrawableComponent*> allComponents);
    void SaveProjectToFile();
    static Project* LoadFromFile(std::string path);
    void IncludeIC(std::string path);
    void IncludeIC(ICDesc icdesc);
};

void to_json(json& j, const Project& p);
void from_json(const json& j, Project& p);