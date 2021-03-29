#pragma once

#include "circuits/circuit_component.hpp"
#include "raylib-cpp/raylib-cpp.hpp"
#include "utils/utility.hpp"

class DrawableComponent : public CircuitComponent {
public:
    Vector2 position;
    Vector2 size;
    Rectangle box;
    const char* text;

    DrawableComponent(Vector2 pos, Vector2 siz, const char* txt, int inps, int outs) : CircuitComponent(inps, outs) {
        this->position = pos;
        this->size = siz;
        this->text = txt;
        this->UpdateBox();
    }

    void UpdateBox() {
        this->box = { position.x, position.y, size.x, size.y };
    }

    float GetIOYPosition(int ios, int index) {
        float start = size.y / 2 - ((ios - 1) * 15.0F) / 2;
        float pos = start + index * 15.0F;
        return pos;
    }

    Vector2 GetInputPosition(int index) {
        Vector2 basePos = position;
        return Vector2{ -10.0F + basePos.x, GetIOYPosition(this->inputs.size(), index) + basePos.y };
    }

    Vector2 GetOutputPosition(int index) {
        Vector2 basePos = position;
        return Vector2{ size.x + 10.0F + basePos.x, GetIOYPosition(this->outputs.size(), index) + basePos.y };
    }

    void DrawInputs() {
        for (int i = 0; i < this->inputs.size(); i++) {
            CircuitInput* inp = this->inputs.at(i);
            Vector2 pos = GetInputPosition(i);

            Color col = inp->GetValue() == LogicValue_HIGH ? BLUE : WHITE;

            DrawCircleV(pos, 7.0F, col);
        }
    }

    void DrawOutputs() {
        for (int i = 0; i < this->outputs.size(); i++) {
            CircuitOutput* out = this->outputs.at(i);
            Vector2 pos = GetOutputPosition(i);

            Color col = out->GetValue() == LogicValue_HIGH ? BLUE : WHITE;

            DrawCircleV(pos, 7.0F, col);
        }
    }

    void Draw() {
        UpdateBox();
        DrawRectanglePro(box, Vector2{ 0.0F, 0.0F }, 0.0F, WHITE);
        DrawInputs();
        DrawOutputs();

        float fontSize = 12.0F;
        Vector2 middleOfBox = Vector2{ box.width / 2.0F, box.height / 2.0F };
        Vector2 textSize = MeasureTextEx(GetFontDefault(), text, fontSize, 1.0F);
        DrawTextEx(GetFontDefault(), this->text, this->position + middleOfBox - (textSize / 2.0F), fontSize, 1.0F, BLACK);
    }

    void DrawSelected() {
        float offset = 2.0F;
        float thickness = 2.0F;
        Rectangle outer = { position.x - offset, position.y - offset, box.width + 2 * offset, box.height + 2 * offset };
        DrawRectangleLinesEx(outer, thickness, BLUE * 0.5F);
    }
};