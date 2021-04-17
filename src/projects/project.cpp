#include "projects/project.hpp"

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

void Project::SaveProjectToFile() {

}

Project* Project::LoadFromFile(std::string path) {
    return new Project{ "default project" };
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