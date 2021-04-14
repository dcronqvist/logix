#pragma once

#include <vector>
#include "integrated/ic_component_desc.hpp"
#include "drawables/drawable_component.hpp"
#include "drawables/drawable_switch.hpp"
#include "drawables/drawable_lamp.hpp"
#include "utils/json.hpp"
using json = nlohmann::json;

class ICDesc {
    public:
    std::vector<ICComponentDesc>* descriptions;
    std::vector<int>* inputs;
    std::vector<int>* outputs;

    ICDesc();
    ICDesc(std::vector<DrawableComponent*> components);
    std::vector<ICComponentDesc>* GenerateDescriptions(std::vector<DrawableComponent*> comps);
    std::vector<CircuitComponent*> GenerateComponents();

    std::vector<int>* GetInputVector(std::vector<DrawableComponent*> components);
    std::vector<int>* GetOutputVector(std::vector<DrawableComponent*> components);
};

void to_json(json& j, const ICDesc& p);
void from_json(const json& j, ICDesc& p);