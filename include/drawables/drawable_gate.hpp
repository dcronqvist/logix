#pragma once

#include "gate-logic/gate_logic.hpp"
#include "drawables/drawable_component.hpp"

class DrawableGate : public DrawableComponent {
public:
    GateLogic* logic;

    DrawableGate(Vector2 pos, GateLogic* gl, int inps) : DrawableComponent(pos, Vector2{ 40, 35 }, gl->GetLogicName(), inps, 1) {
        this->logic = gl;
    }

    void PerformLogic() {
        this->outputs.at(0)->SetValue(logic->PerformGateLogic(this->inputs));
    }
};