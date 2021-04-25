#pragma once

#include "circuits/circuit_component.hpp"
#include "raylib-cpp/raylib-cpp.hpp"
#include "utils/utility.hpp"
#include "drawables/drawable_component.hpp"
#include <string>
#include <iostream>
#include <iomanip>
#include <sstream>

class DrawableHexViewer : public DrawableComponent {
public:
    int value;
    int bits;

    DrawableHexViewer(int bits, Vector2 pos, std::vector<int>* inps) : DrawableComponent(pos, Vector2{ (40.0F * std::max(1.0F, std::ceil((float)bits / 4.0F))), 50 }, "", inps, new std::vector<int>{}) {
        this->value = 0;
        this->bits = bits;
    }

    void PerformLogic() {
        this->value = 0;

        if (IsKeyPressed(KEY_K)) {
            int x = 2;
        }

        if (this->inputs.size() == 1 && this->inputs.at(0)->bits > 1) {
            CircuitInput* ci = this->inputs.at(0);
            for (int i = 0; i < ci->bits; i++) {
                value += ci->GetValue(i) == LogicValue_HIGH ? (0b1 << (ci->bits - i - 1)) : 0;
            }
        }
        else {
            for (int i = 0; i < this->inputs.size(); i++) {
                value += this->inputs.at(i)->GetValue(0) == LogicValue_HIGH ? (0b1 << (this->inputs.size() - i - 1)) : 0;
            }
        }

        this->text = IntToHexString(this->value);
    }

    std::string IntToHexString(int i) {
        std::stringstream stream;
        stream << std::setfill('0') << std::setw((int)std::ceil((float)bits / 4.0F)) << std::hex << std::uppercase << i;
        return stream.str();
    }

    void Draw(Vector2 mousePosInWorld) {
        UpdateBox();
        DrawRectanglePro(box, Vector2{ 0.0F, 0.0F }, 0.0F, WHITE);
        DrawInputs(mousePosInWorld);
        DrawOutputs(mousePosInWorld);

        float fontSize = 40.0F;
        Vector2 middleOfBox = Vector2{ box.width / 2.0F, box.height / 2.0F };
        Vector2 textSize = MeasureTextEx(GetFontDefault(), text.c_str(), fontSize, 1.0F);
        DrawTextEx(GetFontDefault(), this->text.c_str(), this->position + middleOfBox - (textSize / 2.0F), fontSize, 1.0F, BLACK);
    }
};