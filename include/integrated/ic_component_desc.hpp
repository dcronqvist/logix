#pragma once

#include "integrated/ic_connection_desc.hpp"
#include <string>
#include <vector>

class ICComponentDesc {
public:
    std::string type;
    std::string id;
    std::vector<ICConnectionDesc> to;
};