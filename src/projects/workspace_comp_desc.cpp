#include "projects/workspace_comp_desc.hpp"

WorkspaceCompDesc::WorkspaceCompDesc() {
    this->type = "";
    this->position = { 0, 0 };
    this->connectedTo = {};
    this->id = "";
    this->ioBits = 0;
    this->inps = {};
    this->desc = NULL;
}

WorkspaceCompDesc::WorkspaceCompDesc(std::string type, Vector2 pos, std::vector<WorkspaceConnDesc> conn, std::string id, int ioBits, std::vector<int> inps, ICDesc* desc) {
    this->type = type;
    this->position = pos;
    this->connectedTo = conn;
    this->id = id;
    this->ioBits = ioBits;
    this->inps = inps;
    this->desc = desc;
}

void to_json(json& j, const WorkspaceCompDesc& p) {
    if (p.desc != NULL) {
        j = json{
            { "type", p.type},
            { "position", p.position},
            {"connectedTo", p.connectedTo},
            { "id", p.id},
            {"ioBits", p.ioBits},
            {"inps", p.inps},
            { "desc", *(p.desc)}
        };
    }
    else {
        j = json{
            { "type", p.type},
            { "position", p.position},
            {"connectedTo", p.connectedTo},
            { "id", p.id},
            {"ioBits", p.ioBits},
            {"inps", p.inps},
            { "desc", {}}
        };
    }
}

void from_json(const json& j, WorkspaceCompDesc& p) {
    j.at("type").get_to(p.type);
    j.at("position").get_to(p.position);
    j.at("connectedTo").get_to(p.connectedTo);
    j.at("id").get_to(p.id);
    j.at("ioBits").get_to(p.ioBits);
    j.at("inps").get_to(p.inps);

    if (!(j.at("desc").is_null())) {
        p.desc = new ICDesc();
        j.at("desc").get_to(*(p.desc));
    }
    else {
        p.desc = NULL;
    }
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