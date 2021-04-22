#pragma once

#include <vector>
#include "utils/json.hpp"
using json = nlohmann::json;

struct WorkspaceConnDesc {
public:
    int to;
    int fromOutputIndex;
    int toInputIndex;
    int bits;
};

void to_json(json& j, const WorkspaceConnDesc& p);
void from_json(const json& j, WorkspaceConnDesc& p);