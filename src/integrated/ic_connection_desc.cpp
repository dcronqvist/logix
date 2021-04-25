#include "integrated/ic_connection_desc.hpp"

void to_json(json& j, const ICConnectionDesc& p) {
    j = json{ {"to", p.to}, {"bits", p.bits}, {"outIndex", p.outIndex}, {"inIndex", p.inIndex} };
}
void from_json(const json& j, ICConnectionDesc& p) {
    j.at("to").get_to(p.to);
    j.at("bits").get_to(p.bits);
    j.at("outIndex").get_to(p.outIndex);
    j.at("inIndex").get_to(p.inIndex);
}