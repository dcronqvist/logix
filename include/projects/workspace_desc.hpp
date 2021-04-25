#pragma once

#include <vector>
#include <tuple>
#include "projects/workspace_comp_desc.hpp"
#include "drawables/drawable_component.hpp"
#include "drawables/drawable_wire.hpp"
#include "drawables/drawable_button.hpp"
#include "drawables/drawable_gate.hpp"
#include "drawables/drawable_hex_viewer.hpp"
#include "drawables/drawable_ic.hpp"
#include "drawables/drawable_lamp.hpp"
#include "drawables/drawable_switch.hpp"
#include "utils/json.hpp"
using json = nlohmann::json;

class WorkspaceDesc {
public:
    std::vector<WorkspaceCompDesc> components;

    WorkspaceDesc();
    WorkspaceDesc(std::vector<DrawableComponent*> comps);
    std::vector<WorkspaceCompDesc> GenerateComponentDescriptions(std::vector<DrawableComponent*> comps);
    std::tuple<std::vector<DrawableComponent*>, std::vector<DrawableWire*>> GenerateDrawables();
};

void to_json(json& j, const WorkspaceDesc& p);
void from_json(const json& j, WorkspaceDesc& p);