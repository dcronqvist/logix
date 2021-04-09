#pragma once

#include "circuits/circuit_component.hpp"
#include "raylib-cpp/raylib-cpp.hpp"
#include "utils/utility.hpp"
#include "drawables/drawable_component.hpp"
#include <vector>

class DrawableButton : public DrawableComponent {
    public:
    LogicValue value;

    DrawableButton(Vector2 pos) : DrawableComponent(pos, Vector2{ 30, 30 }, "0", new std::vector<int>{ 0 }, new std::vector<int>{ 1 }) {
        value = LogicValue_LOW;
    }

    void PerformLogic() {
        this->outputs.at(0)->SetValues(value);
    }

    void Draw(Vector2 mousePosInWorld) {
        UpdateBox();

        DrawRectangleRounded(box, 0.5F, 5, WHITE);
        DrawOutputs(mousePosInWorld);

        Color col = this->value == LogicValue_HIGH ? BLUE : RAYWHITE;
        float offset = 1.0F;
        Rectangle r = Rectangle{ this->position.x + offset, this->position.y + offset, this->box.width - 2 * offset, this->box.height - 2 * offset };
        DrawRectangleRounded(r, 0.5F, 5, col);

        float fontSize = 12.0F;
        Vector2 middleOfBox = Vector2{ box.width / 2.0F, box.height / 2.0F };
        Vector2 textSize = MeasureTextEx(GetFontDefault(), text, fontSize, 1.0F);
        DrawTextEx(GetFontDefault(), this->text, this->position + middleOfBox - (textSize / 2.0F), fontSize, 1.0F, BLACK);
    }

    void Update(Vector2 mousePosInWorld) {
        this->value = LogicValue_LOW;
        this->text = "0";
        if (IsMouseButtonDown(MOUSE_RIGHT_BUTTON) && CheckCollisionPointRec(mousePosInWorld, this->box)) {
            this->value = LogicValue_HIGH;
            this->text = "1";
        }
    }
};