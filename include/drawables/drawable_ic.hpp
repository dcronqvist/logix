#pragma once

#include "drawables/drawable_component.hpp"
#include "integrated/ic_desc.hpp"
#include "minimals/minimal_switch.hpp"
#include "minimals/minimal_lamp.hpp"
#include "integrated/ic_input.hpp"
#include "integrated/ic_output.hpp"
#include <vector>
#include <string>

class DrawableIC : public DrawableComponent {
    public:
    ICDesc description;
    std::vector<CircuitComponent*> components;

    DrawableIC(Vector2 pos, ICDesc desc) : DrawableComponent(pos, 0.0F, desc.name.c_str(), std::vector<CircuitInput*>{}, std::vector<CircuitOutput*>{}) {
        this->description = desc;
        this->components = desc.GenerateComponents();
        this->inputs = desc.GenerateICInputs(this->components);
        this->outputs = desc.GenerateICOutputs(this->components);
        this->size = Vector2{ CalculateWidth(desc), CalculateMinHeight() };
        this->UpdateBox();
    }

    std::vector<std::string> GetInputIDs() {
        std::vector<std::string> ids = {};
        for (int i = 0; i < this->inputs.size(); i++) {
            ids.push_back(dynamic_cast<ICInput*>(this->inputs.at(i))->id);
        }
        return ids;
    }

    std::vector<std::string> GetOutputIDs() {
        std::vector<std::string> ids = {};
        for (int i = 0; i < this->outputs.size(); i++) {
            ids.push_back(dynamic_cast<ICOutput*>(this->outputs.at(i))->id);
        }
        return ids;
    }

    float CalculateWidth(ICDesc desc) {
        float distBetweenIOAndText = 50.0F;
        std::vector<std::string> inps = GetInputIDs();
        std::vector<std::string> outs = GetOutputIDs();

        Vector2 textMeasure = MeasureTextEx(GetFontDefault(), desc.name.c_str(), 10.0F, 1.0F);
        float maxIOWidth = 0.0F;
        for (int i = 0; i < inps.size(); i++) {
            if (MeasureText(inps.at(i).c_str(), 10.0F) > maxIOWidth) { maxIOWidth = (float)MeasureText(inps.at(i).c_str(), 10.0F); }
        }
        for (int i = 0; i < outs.size(); i++) {
            if (MeasureText(outs.at(i).c_str(), 10.0F) > maxIOWidth) { maxIOWidth = (float)MeasureText(outs.at(i).c_str(), 10.0F); }
        }

        return maxIOWidth * 2.0F + distBetweenIOAndText + textMeasure.x;
    }

    void PerformLogic();

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

            const char* inputId = dynamic_cast<ICInput*>(this->inputs.at(i))->id.c_str();
            Vector2 inputIdTextMeasure = MeasureTextEx(GetFontDefault(), inputId, 10.0F, 1.0F);
            DrawTextEx(GetFontDefault(), inputId, pos + Vector2{ 15.0F, -inputIdTextMeasure.y / 2.0F }, 10.0F, 1.0F, BLACK);
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

            const char* outputId = dynamic_cast<ICOutput*>(this->outputs.at(i))->id.c_str();
            Vector2 outputIdMeasure = MeasureTextEx(GetFontDefault(), outputId, 10.0F, 1.0F);
            DrawTextEx(GetFontDefault(), outputId, pos - Vector2{ 15.0F + outputIdMeasure.x, outputIdMeasure.y / 2.0F }, 10.0F, 1.0F, BLACK);
        }
    }
    /*
    void Draw(Vector2 mousePosInWorld) {
        UpdateBox();
        DrawRectanglePro(box, Vector2{ 0.0F, 0.0F }, 0.0F, WHITE);
        DrawInputs(mousePosInWorld);
        DrawOutputs(mousePosInWorld);

        float fontSize = 12.0F;
        Vector2 middleOfBox = Vector2{ box.width / 2.0F, box.height / 2.0F };
        Vector2 textSize = MeasureTextEx(GetFontDefault(), text, fontSize, 1.0F);
        DrawTextEx(GetFontDefault(), this->text, this->position + middleOfBox - (textSize / 2.0F), fontSize, 1.0F, BLACK);
    }*/
};