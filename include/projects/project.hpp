#pragma once

#include "integrated/ic_desc.hpp"
#include <vector>
#include <string>
#include <iostream>
#include <fstream>

class Project {
public:
    std::vector<std::string> includedICs;
    std::string name;

    Project(std::string name);

    std::vector<ICDesc> GetAllIncludedICs();
    void SaveProjectToFile();
    static Project* LoadFromFile(std::string path);
    void IncludeIC(std::string path);
    void IncludeIC(ICDesc icdesc);
};