#pragma once

#include "utils/json.hpp"
using json = nlohmann::json;

class ICConnectionDesc {
    public:
    int to;
    int bits;
    int outIndex;
    int inIndex;
};

void to_json(json& j, const ICConnectionDesc& p);
void from_json(const json& j, ICConnectionDesc& p);