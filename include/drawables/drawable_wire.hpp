#pragma once

#include "circuits/circuit_wire.hpp"
#include "drawables/drawable_component.hpp"
#include "raylib-cpp/raylib-cpp.hpp"
#include <vector>

class DrawableWire : public CircuitWire {
    public:
    DrawableComponent* from;
    DrawableComponent* to;
    std::vector<Vector2> points;

    int fromIndex;
    int toIndex;

    DrawableWire(DrawableComponent* fr, int fi, DrawableComponent* t, int ti, int bits) : CircuitWire(bits) {
        this->from = fr;
        this->to = t;
        this->fromIndex = fi;
        this->toIndex = ti;
    }

    void Draw() {
        float thickness = 3.0F;

        Color col = BLUE * this->from->GetOutputFromIndex(this->fromIndex)->GetHIGHFraction();
        //DrawLineBezier(from->GetOutputPosition(fromIndex), to->GetInputPosition(toIndex), thickness, col);
        DrawLineEx(from->GetOutputPosition(fromIndex), to->GetInputPosition(toIndex), thickness, WHITE);
        DrawLineEx(from->GetOutputPosition(fromIndex), to->GetInputPosition(toIndex), thickness, col);
    }

    bool IsPositionOnLine(Vector2 position) {
        Vector2 start = from->GetOutputPosition(fromIndex);
        Vector2 end = to->GetInputPosition(toIndex);

        float k = (end.y - start.y) / (end.x - start.x);
        if (abs((k * (position.x - from->GetOutputPosition(fromIndex).x)) - (position.y - from->GetOutputPosition(fromIndex).y)) < 3.0F) {
            return true;
        }
        return false;
    }
};