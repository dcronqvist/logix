#include "projects/workspace_comp_desc.hpp"

void to_json(json& j, const WorkspaceCompDesc& p) {
    j = json{
        { "type", p.type},
        { "position", p.position},
        {"connectedTo", p.connectedTo},
        { "id", p.id},
        {"ioBits", p.ioBits},
        {"inps", p.inps}
    };
}

void from_json(const json& j, WorkspaceCompDesc& p) {
    j.at("type").get_to(p.type);
    j.at("position").get_to(p.position);
    j.at("connectedTo").get_to(p.connectedTo);
    j.at("id").get_to(p.id);
    j.at("ioBits").get_to(p.ioBits);
    j.at("inps").get_to(p.inps);

}

void to_json(json& j, const Vector2& p) {
    j = json{
        { "x", p.x},
        { "y", p.y}
    };
}

void from_json(const json& j, Vector2& p) {
    j.at("x").get_to(p.x);
    j.at("y").get_to(p.y);
}