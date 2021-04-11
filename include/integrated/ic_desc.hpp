#pragma once

#include <vector>
#include "integrated/ic_component_desc.hpp"
#include "drawables/drawable_component.hpp"

class ICDesc {
public:
    std::vector<ICComponentDesc>* descriptions;
    int inputs;
    int outputs;

    ICDesc(std::vector<DrawableComponent*> components) {
        this->descriptions = GenerateDescriptions(components);
    }
    std::vector<ICComponentDesc>* GenerateDescriptions(std::vector<DrawableComponent*> comps);
};