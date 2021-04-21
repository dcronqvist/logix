#include "projects/workspace_conn_desc.hpp"

void to_json(json& j, const WorkspaceConnDesc& p) {
    j = json{ { "to", p.to}, {"fromOutputIndex", p.fromOutputIndex}, {"toInputIndex", p.toInputIndex}, {"bits", p.bits} };
}

void from_json(const json& j, WorkspaceConnDesc& p) {
    j.at("to").get_to(p.to);
    j.at("fromOutputIndex").get_to(p.fromOutputIndex);
    j.at("toInputIndex").get_to(p.toInputIndex);
    j.at("bits").get_to(p.bits);
}