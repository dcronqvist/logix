#pragma once

#include "gate-logic/gate_logic.hpp"
#include "drawables/drawable_component.hpp"
#include <vector>

class DrawableGate : public DrawableComponent {
    public:
    GateLogic* logic;

    DrawableGate(Vector2 pos, GateLogic* gl, std::vector<int>* inps) : DrawableComponent(pos, Vector2{ 40, 35 }, gl->GetLogicName(), inps, new std::vector<int>{ 1 }) {
        this->logic = gl;
    }

    void PerformLogic() {
        this->outputs.at(0)->SetValues(logic->PerformGateLogic(this->inputs));
    }
};