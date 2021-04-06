#pragma once

#include "circuits/circuit_component.hpp"
#include "raylib-cpp/raylib-cpp.hpp"
#include "utils/utility.hpp"
#include "drawables/drawable_component.hpp"

class DrawableLamp : public DrawableComponent {
    public:
    LogicValue value;

    DrawableLamp(Vector2 pos) : DrawableComponent(pos, Vector2{30, 30}, "0", 1, 0) {
        value = LogicValue_LOW;
    }

    void PerformLogic() {
        this->value = this->inputs.at(0)->GetValue();
        this->text = this->value == LogicValue_HIGH ? "1" : "0";
    }

    void Draw(Vector2 mousePosInWorld) {
        UpdateBox();

        Vector2 middle = position + Vector2{box.width / 2.0F, box.height / 2.0F};
        float radius = box.width / 2.0F;

        DrawCircleV(middle, radius, WHITE);
        DrawInputs(mousePosInWorld);

        Color col = this->value == LogicValue_HIGH ? BLUE : RAYWHITE;
        float offset = 1.0F;
        DrawCircleV(middle, radius - offset, col);

        float fontSize = 12.0F;
        Vector2 middleOfBox = Vector2{ box.width / 2.0F, box.height / 2.0F };
        Vector2 textSize = MeasureTextEx(GetFontDefault(), text, fontSize, 1.0F);
        DrawTextEx(GetFontDefault(), this->text, this->position + middleOfBox - (textSize / 2.0F), fontSize, 1.0F, BLACK);
    }
};