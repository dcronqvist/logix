#pragma once

#include "circuits/circuit_wire.hpp"
#include "drawables/drawable_component.hpp"
#include "raylib-cpp/raylib-cpp.hpp"

class DrawableWire : public CircuitWire {
    public:
    DrawableComponent* from;
    DrawableComponent* to;

    int fromIndex;
    int toIndex;

    DrawableWire(DrawableComponent* fr, int fi, DrawableComponent* t, int ti) : CircuitWire() {
        this->from = fr;
        this->to = t;
        this->fromIndex = fi;
        this->toIndex = ti;
    }

    void Draw() {
        float thickness = 3.0F;

        Color col = this->GetValue() == LogicValue_HIGH ? BLUE : WHITE;
        DrawLineBezier(from->GetOutputPosition(fromIndex), to->GetInputPosition(toIndex), thickness, col);
    }
};