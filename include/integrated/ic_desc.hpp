#pragma once

#include <vector>
#include "integrated/ic_component_desc.hpp"
#include "drawables/drawable_component.hpp"
#include "drawables/drawable_switch.hpp"
#include "drawables/drawable_lamp.hpp"
#include "utils/json.hpp"
#include <string>
using json = nlohmann::json;

class ICDesc {
    public:
    std::vector<ICComponentDesc>* descriptions;
    std::vector<std::vector<std::string>> inputs;
    std::vector<std::vector<std::string>> outputs;
    std::string name;

    ICDesc() {};
    ICDesc(std::string name);
    ICDesc(std::string name, std::vector<DrawableComponent*> components, std::vector<std::vector<std::string>> inps, std::vector<std::vector<std::string>> outs);
    std::vector<ICComponentDesc>* GenerateDescriptions(std::vector<DrawableComponent*> comps);
    std::vector<CircuitComponent*> GenerateComponents();
    std::vector<CircuitInput*> GenerateICInputs(std::vector<CircuitComponent*> comps);
    std::vector<CircuitOutput*> GenerateICOutputs(std::vector<CircuitComponent*> comps);
};

void to_json(json& j, const ICDesc& p);
void from_json(const json& j, ICDesc& p);