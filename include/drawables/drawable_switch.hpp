#pragma once

#include "circuits/circuit_component.hpp"
#include "raylib-cpp/raylib-cpp.hpp"
#include "utils/utility.hpp"
#include "drawables/drawable_component.hpp"

class DrawableSwitch : public DrawableComponent {
    public:
    int bits;
    std::vector<LogicValue> values;

    DrawableSwitch(Vector2 pos, int bits) : DrawableComponent(pos, Vector2{ bits * 30.0F, 30 }, "", new std::vector<int>{}, new std::vector<int>{ bits }) {
        this->bits = bits;
        this->values = {};
        for (int i = 0; i < bits; i++) {
            this->values.push_back(LogicValue_LOW);
        }
    }

    void PerformLogic() {
        this->outputs.at(0)->SetValues(this->values);
    }

    void Draw(Vector2 mousePosInWorld) {
        UpdateBox();
        DrawRectanglePro(box, Vector2{ 0.0F, 0.0F }, 0.0F, WHITE);
        DrawInputs(mousePosInWorld);
        DrawOutputs(mousePosInWorld);

        for (int i = 0; i < this->bits; i++) {
            Color col = this->values.at(i) == LogicValue_HIGH ? BLUE : RAYWHITE;
            float offset = 1.0F;
            Rectangle r = Rectangle{ this->position.x + offset + 30 * i, this->position.y + offset, (this->box.width / this->bits) - 2 * offset, (this->box.height) - 2 * offset };
            DrawRectanglePro(r, Vector2{ 0.0F, 0.0F }, 0.0F, col);
        }
    }

    void Update(Vector2 mousePosInWorld) {

        for (int i = 0; i < this->bits; i++) {
            Color col = this->values.at(i) == LogicValue_HIGH ? BLUE : RAYWHITE;
            float offset = 1.0F;
            Rectangle r = Rectangle{ this->position.x + offset + 30 * i, this->position.y + offset, (this->box.width / this->bits) - 2 * offset, (this->box.height) - 2 * offset };

            if (IsMouseButtonPressed(MOUSE_RIGHT_BUTTON) && CheckCollisionPointRec(mousePosInWorld, r)) {
                LogicValue oldVal = this->values.at(i);

                this->values.erase(this->values.begin() + i);

                this->values.emplace(this->values.begin() + i, oldVal == LogicValue_HIGH ? LogicValue_LOW : LogicValue_HIGH);
            }
        }

    }
};