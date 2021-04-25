#pragma once

#include <vector>
#include "projects/workspace_conn_desc.hpp"
#include "raylib-cpp/raylib-cpp.hpp"
#include "integrated/ic_desc.hpp"
#include <string>
#include "utils/json.hpp"
using json = nlohmann::json;


class WorkspaceCompDesc {
public:
    std::string type;
    Vector2 position;
    std::vector<WorkspaceConnDesc> connectedTo;
    std::string id;
    int ioBits;
    std::vector<int> inps;
    ICDesc* desc;

    WorkspaceCompDesc();
    WorkspaceCompDesc(std::string type, Vector2 pos, std::vector<WorkspaceConnDesc> conn, std::string id, int ioBits, std::vector<int> inps, ICDesc* desc);
};

void to_json(json& j, const WorkspaceCompDesc& p);
void from_json(const json& j, WorkspaceCompDesc& p);

void to_json(json& j, const Vector2& p);
void from_json(const json& j, Vector2& p);