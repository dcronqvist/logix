#include "projects/project.hpp"

void to_json(json& j, const Project& p) {
    j = json{ {"name", p.name}, {"includedICs", p.includedICs}, {"workspace", p.workspace} };
}

void from_json(const json& j, Project& p) {
    j.at("name").get_to(p.name);
    j.at("includedICs").get_to(p.includedICs);
    j.at("workspace").get_to(p.workspace);
}

Project::Project(std::string name) {
    this->name = name;
    this->includedICs = {};
}

std::vector<ICDesc> Project::GetAllIncludedICs() {
    std::vector<ICDesc> descs = {};
    for (int i = 0; i < this->includedICs.size(); i++) {
        std::string path = this->includedICs.at(i);
        std::ifstream fil(path);
        json j;
        fil >> j;
        descs.push_back(j.get<ICDesc>());
        fil.close();
    }
    return descs;
}

void Project::SaveProjectToFile(std::string filePath) {
    json j = *this;
    std::ofstream o(filePath);
    o << j << std::endl;
    o.close();
}

void Project::SaveWorkspace(std::vector<DrawableComponent*> allComponents) {
    this->workspace = WorkspaceDesc{ allComponents };
}

void Project::IncludeIC(std::string path) {
    this->includedICs.push_back(path);
}

void Project::IncludeIC(ICDesc icdesc) {
    std::ofstream o("ic/" + icdesc.name + ".ic");
    json j = icdesc;
    o << j << std::endl;
    o.close();
    this->IncludeIC("ic/" + icdesc.name + ".ic");
}