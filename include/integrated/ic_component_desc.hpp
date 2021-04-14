#pragma once

#include "integrated/ic_connection_desc.hpp"
#include <string>
#include <vector>
#include "utils/json.hpp"
using json = nlohmann::json;

class ICComponentDesc {
public:
    std::string type;
    std::string id;
    std::vector<ICConnectionDesc> to;
};

void to_json(json& j, const ICComponentDesc& p);
void from_json(const json& j, ICComponentDesc& p);