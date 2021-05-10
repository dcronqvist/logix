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
    return this->includedICs;
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

void Project::IncludeIC(ICDesc icdesc) {
    this->includedICs.push_back(icdesc);
}

void Project::RemoveIC(std::string name) {
    std::vector<ICDesc> withoutOld = {};
    for (int i = 0; i < this->includedICs.size(); i++) {
        if (this->includedICs.at(i).name != name) {
            withoutOld.push_back(this->includedICs.at(i));
        }
    }
    this->includedICs = withoutOld;
}