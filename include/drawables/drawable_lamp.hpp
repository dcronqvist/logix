#pragma once

#include "circuits/circuit_component.hpp"
#include "raylib-cpp/raylib-cpp.hpp"
#include "utils/utility.hpp"
#include "drawables/drawable_component.hpp"

class DrawableLamp : public DrawableComponent {
public:
    LogicValue value;
    std::string* id;

    DrawableLamp(Vector2 pos) : DrawableComponent(pos, Vector2{ 30, 30 }, "0", new std::vector<int>{ 1 }, new std::vector<int>{}) {
        value = LogicValue_LOW;
        this->id = new std::string{ "" };
    }

    void PerformLogic() {
        this->value = this->inputs.at(0)->GetValue(0);
        this->text = this->value == LogicValue_HIGH ? "1" : "0";
    }

    void Draw(Vector2 mousePosInWorld) {
        UpdateBox();

        Vector2 middle = position + Vector2{ box.width / 2.0F, box.height / 2.0F };
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

        DrawText(this->id->c_str(), box.x, box.y, 10.0F, BLACK);
    }
};