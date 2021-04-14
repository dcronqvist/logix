#include "integrated/ic_component_desc.hpp"

void to_json(json& j, const ICComponentDesc& p) {
    j = json{ {"type", p.type}, {"id", p.id}, {"inputs", *p.inputs }, {"to", p.to} };
}
void from_json(const json& j, ICComponentDesc& p) {
    j.at("type").get_to(p.type);
    j.at("id").get_to(p.id);
    j.at("inputs").get_to(*(p.inputs));
    j.at("to").get_to(p.to);
}