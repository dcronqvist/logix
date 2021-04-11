#pragma once

#include "circuits/circuit_component.hpp"
#include "raylib-cpp/raylib-cpp.hpp"
#include "utils/utility.hpp"
#include <vector>

const float RADIUS = 7.0F;
const float Y_DIST = 20.0F;

class DrawableComponent : public CircuitComponent {
public:
    Vector2 position;
    Vector2 size;
    Rectangle box;
    const char* text;

    DrawableComponent(Vector2 pos, Vector2 siz, const char* txt, std::vector<int>* inps, std::vector<int>* outs) : CircuitComponent(inps, outs) {
        this->position = pos;
        this->size = siz;
        this->text = txt;
        this->UpdateBox();
    }

    DrawableComponent(Vector2 pos, float width, const char* txt, std::vector<int>* inps, std::vector<int>* outs) : CircuitComponent(inps, outs) {
        this->position = pos;
        this->size = { width, CalculateMinHeight() };
        this->text = txt;
        this->UpdateBox();
    }

    void UpdateBox() {
        this->box = { position.x, position.y, size.x, size.y };
    }

    float GetIOYPosition(int ios, int index) {
        float dist = Y_DIST;
        float start = size.y / 2 - ((ios - 1) * dist) / 2;
        float pos = start + index * dist;
        return pos;
    }

    float CalculateMinHeight() {
        return Y_DIST * std::max(this->inputs.size(), this->outputs.size());
    }

    Vector2 GetInputPosition(int index) {
        Vector2 basePos = position;
        return Vector2{ -10.0F + basePos.x, GetIOYPosition(this->inputs.size(), index) + basePos.y };
    }

    Vector2 GetOutputPosition(int index) {
        Vector2 basePos = position;
        return Vector2{ size.x + 10.0F + basePos.x, GetIOYPosition(this->outputs.size(), index) + basePos.y };
    }

    int GetInputIndexFromPosition(Vector2 position) {
        for (int i = 0; i < this->inputs.size(); i++) {
            Vector2 inputPos = GetInputPosition(i);
            Vector2 diff = inputPos - position;

            if (Vector2Length(diff) < RADIUS) {
                return i;
            }
        }
        return -1;
    }

    int GetOutputIndexFromPosition(Vector2 position) {
        for (int i = 0; i < this->outputs.size(); i++) {
            Vector2 outputPos = GetOutputPosition(i);
            Vector2 diff = outputPos - position;

            if (Vector2Length(diff) < RADIUS) {
                return i;
            }
        }
        return -1;
    }

    void DrawInputs(Vector2 mousePosInWorld) {
        for (int i = 0; i < this->inputs.size(); i++) {
            CircuitInput* inp = this->inputs.at(i);
            Vector2 pos = GetInputPosition(i);
            DrawLineEx(pos, pos + Vector2{ 10.0F, 0 }, 1.5F, WHITE);

            if (inp->bits == 1) {
                Color col = (inp->GetValues().at(0)) == LogicValue_HIGH ? BLUE : WHITE;

                if (Vector2Length(pos - mousePosInWorld) < RADIUS) {
                    col = ORANGE;
                }

                DrawCircleV(pos, RADIUS, col);
            }
            else {
                Color col = BLUE * inp->GetHIGHFraction();

                if (Vector2Length(pos - mousePosInWorld) < RADIUS) {
                    col = ORANGE;
                }

                DrawCircleV(pos, RADIUS, WHITE);
                DrawCircleV(pos, RADIUS, col);

                const char* bitText = std::to_string(inp->bits).c_str();
                Vector2 textMeasure = MeasureTextEx(GetFontDefault(), bitText, 10.0F, 1.0F);
                DrawTextEx(GetFontDefault(), bitText, pos - textMeasure / 2.0F, 10.0F, 1.0F, BLACK);
            }
        }
    }

    void DrawOutputs(Vector2 mousePosInWorld) {
        for (int i = 0; i < this->outputs.size(); i++) {
            CircuitOutput* out = this->outputs.at(i);
            Vector2 pos = GetOutputPosition(i);
            DrawLineEx(pos, pos - Vector2{ 10.0F, 0 }, 1.5F, WHITE);

            if (out->bits == 1) {
                Color col = out->GetValues().at(0) == LogicValue_HIGH ? BLUE : WHITE;

                if (Vector2Length(pos - mousePosInWorld) < RADIUS) {
                    col = ORANGE;
                }

                DrawCircleV(pos, RADIUS, col);
            }
            else {
                Color col = BLUE * out->GetHIGHFraction();

                if (Vector2Length(pos - mousePosInWorld) < RADIUS) {
                    col = ORANGE;
                }

                DrawCircleV(pos, RADIUS, WHITE);
                DrawCircleV(pos, RADIUS, col);

                const char* bitText = std::to_string(out->bits).c_str();
                Vector2 textMeasure = MeasureTextEx(GetFontDefault(), bitText, 10.0F, 1.0F);
                DrawTextEx(GetFontDefault(), bitText, pos - textMeasure / 2.0F, 10.0F, 1.0F, BLACK);
            }
        }
    }

    virtual void Draw(Vector2 mousePosInWorld) {
        UpdateBox();
        DrawRectanglePro(box, Vector2{ 0.0F, 0.0F }, 0.0F, WHITE);
        DrawInputs(mousePosInWorld);
        DrawOutputs(mousePosInWorld);

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

    virtual void Update(Vector2 mousePosInWorld) {};
};