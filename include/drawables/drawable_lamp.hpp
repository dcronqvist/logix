#pragma once

#include "circuits/circuit_component.hpp"
#include "raylib-cpp/raylib-cpp.hpp"
#include "utils/utility.hpp"
#include "drawables/drawable_component.hpp"

class DrawableLamp : public DrawableComponent {
public:
    int bits;
    std::vector<LogicValue> values;
    std::string* id;

    DrawableLamp(Vector2 pos, int bits) : DrawableComponent(pos, Vector2{ 30.0F * (float)bits, 30 }, "", new std::vector<int>{ bits }, new std::vector<int>{}) {
        this->values = {};
        for (int i = 0; i < bits; i++) {
            this->values.push_back(LogicValue_LOW);
        }
        this->id = new std::string{ "" };
        this->bits = bits;
    }

    void PerformLogic() {
        this->values = this->inputs.at(0)->GetValues();
        //this->text = this->value == LogicValue_HIGH ? "1" : "0";
    }

    void Draw(Vector2 mousePosInWorld) {
        UpdateBox();
        DrawInputs(mousePosInWorld);
        Vector2 middle = position + Vector2{ (box.width / (float)bits) / 2.0F, box.height / 2.0F };
        float radius = (box.width / (float)bits) / 2.0F;

        for (int i = 0; i < this->bits; i++) {

            DrawCircleV(middle + Vector2{ 2 * radius * i }, radius, WHITE);

            Color col = this->values.at(i) == LogicValue_HIGH ? BLUE : RAYWHITE;
            float offset = 1.0F;
            DrawCircleV(middle + Vector2{ 2 * radius * i }, radius - offset, col);
        }
        DrawText(this->id->c_str(), box.x, box.y, 10.0F, BLACK);
    }
};