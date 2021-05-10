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
    std::vector<ICDesc> includedICs;
    std::string name;
    WorkspaceDesc workspace;

    Project() {
        this->includedICs = {};
        this->name = "";
        this->workspace = {};
    }
    Project(std::string name);

    std::vector<ICDesc> GetAllIncludedICs();
    void SaveWorkspace(std::vector<DrawableComponent*> allComponents);
    void SaveProjectToFile(std::string path);
    void IncludeIC(ICDesc icdesc);
};

void to_json(json& j, const Project& p);
void from_json(const json& j, Project& p);