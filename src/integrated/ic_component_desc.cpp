#include "integrated/ic_component_desc.hpp"
#include "integrated/ic_desc.hpp"

ICComponentDesc::ICComponentDesc(std::string type, std::string id, std::vector<ICConnectionDesc> to, std::vector<int>* inps) {
    this->type = type;
    this->id = id;
    this->to = to;
    this->inputs = inps;
    this->desc = NULL;
}

ICComponentDesc::ICComponentDesc() {
    this->type = "";
    this->id = "";
    this->to = {};
    this->inputs = new std::vector<int>();
    this->desc = NULL;
}

void to_json(json& j, const ICComponentDesc& p) {
    if (p.desc != NULL) {
        j = json{ {"type", p.type}, {"id", p.id}, {"inputs", *p.inputs }, {"to", p.to}, {"ic", *(p.desc) } };
    }
    else {
        j = json{ {"type", p.type}, {"id", p.id}, {"inputs", *p.inputs }, {"to", p.to}, {"ic", {} } };
    }
}
void from_json(const json& j, ICComponentDesc& p) {
    j.at("type").get_to(p.type);
    j.at("id").get_to(p.id);
    j.at("inputs").get_to(*(p.inputs));
    j.at("to").get_to(p.to);

    if (!(j.at("ic").is_null())) {
        j.at("ic").get_to(*(p.desc));
    }
}