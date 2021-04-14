#include "integrated/ic_desc.hpp"
#include "drawables/drawable_gate.hpp"
#include "drawables/drawable_wire.hpp"
#include "drawables/drawable_lamp.hpp"
#include "drawables/drawable_switch.hpp"
#include "minimals/minimal_gate.hpp"
#include "minimals/minimal_switch.hpp"
#include "minimals/minimal_lamp.hpp"
#include "gate-logic/and_gate_logic.hpp"
#include <vector>
#include <string>

ICDesc::ICDesc() {
    this->descriptions = new std::vector<ICComponentDesc>();
    this->inputs = new std::vector<int>();
    this->outputs = new std::vector<int>();
}

ICDesc::ICDesc(std::vector<DrawableComponent*> components) {
    this->descriptions = GenerateDescriptions(components);
    this->inputs = GetInputVector(components);
    this->outputs = GetOutputVector(components);
}

int GetIndexOfComponent(std::vector<DrawableComponent*> comps, DrawableComponent* comp) {
    // Should return index of comp in comps,
    // if not found, return -1
    auto it = std::find(comps.begin(), comps.end(), comp);
    if (it == comps.end()) { return -1; }

    return it - comps.begin();
}

std::vector<ICComponentDesc>* ICDesc::GenerateDescriptions(std::vector<DrawableComponent*> comps) {
    std::vector<ICComponentDesc>* vicc = new std::vector<ICComponentDesc>{};

    for (int i = 0; i < comps.size(); i++) {
        DrawableComponent* dc = comps.at(i);

        const char* type;
        const char* id;

        if (dynamic_cast<DrawableGate*>(dc) != NULL) { // It is a drawablegate
            DrawableGate* gate = dynamic_cast<DrawableGate*>(dc);
            type = gate->logic->GetLogicName();
        }
        if (dynamic_cast<DrawableSwitch*>(dc) != NULL) {
            type = "Switch";
            id = std::to_string(i).c_str();
        }
        if (dynamic_cast<DrawableLamp*>(dc) != NULL) {
            type = "Lamp";
            id = std::to_string(i).c_str();
        }

        // TODO: Add all other drawable component types

        std::vector<ICConnectionDesc> to = {};

        for (int j = 0; j < dc->outputs.size(); j++) {
            CircuitOutput* co = dc->outputs.at(j);

            for (int k = 0; k < co->GetSignals().size(); k++) {
                CircuitWire* cw = co->GetSignals().at(k);
                DrawableWire* wire = dynamic_cast<DrawableWire*>(cw);

                if (std::find(comps.begin(), comps.end(), wire->to) != comps.end()) {
                    int indexOf = GetIndexOfComponent(comps, wire->to);

                    if (indexOf == -1) {
                        return NULL;
                    }

                    ICConnectionDesc iccd = { indexOf, wire->bits, wire->fromIndex, wire->toIndex };
                    to.push_back(iccd);
                }
            }
        }

        ICComponentDesc iccc = { type, id, to, dc->GetInputVector() };

        vicc->push_back(iccc);
    }
    return vicc;
}

std::vector<CircuitComponent*> ICDesc::GenerateComponents() {
    std::vector<CircuitComponent*> comps = {};

    for (int i = 0; i < this->descriptions->size(); i++) {
        ICComponentDesc compDesc = this->descriptions->at(i);
        CircuitComponent* comp;

        if (compDesc.type != "Switch" && compDesc.type != "Lamp") {
            comp = new MinimalGate(GetGateLogic(compDesc.type.c_str()), compDesc.inputs);
        }
        else if (compDesc.type == "Switch") {
            // TODO: LOOK AT FIRST CONNECTION AND USE THAT AMOUNT OF BITS
            // This works simply because a switch can only ever have 1 output. 
            comp = new MinimalSwitch(compDesc.to.at(0).bits, compDesc.id);
        }
        else if (compDesc.type == "Lamp") {
            comp = new MinimalLamp(compDesc.id);
        }

        comps.push_back(comp);
    }

    for (int i = 0; i < comps.size(); i++) {
        ICComponentDesc compDesc = this->descriptions->at(i);

        for (int j = 0; j < compDesc.to.size(); j++) {
            ICConnectionDesc connection = compDesc.to.at(j);
            // For each connection going from this component, to another
            CircuitWire* wire = new CircuitWire(connection.bits);
            comps.at(i)->AddOutputWire(connection.outIndex, wire);
            comps.at(connection.to)->SetInputWire(connection.inIndex, wire);
        }
    }

    return comps;
}

std::vector<int>* ICDesc::GetInputVector(std::vector<DrawableComponent*> components) {
    std::vector<int>* iv = new std::vector<int>();

    for (int i = 0; i < components.size(); i++) {
        DrawableSwitch* ms = dynamic_cast<DrawableSwitch*>(components.at(i));
        if (ms != NULL) {
            iv->push_back(ms->bits);
        }
    }
    return iv;
}

std::vector<int>* ICDesc::GetOutputVector(std::vector<DrawableComponent*> components) {
    std::vector<int>* iv = new std::vector<int>();

    for (int i = 0; i < components.size(); i++) {
        DrawableLamp* ms = dynamic_cast<DrawableLamp*>(components.at(i));
        if (ms != NULL) {
            // For now this is set to 1
            // TODO: ALLOW FOR MULTIBIT OUTPUTS SOMEHOW
            iv->push_back(1);
        }
    }
    return iv;
}

void to_json(json& j, const ICDesc& p) {
    j = json{ {"inputs", *(p.inputs)}, {"outputs", *(p.outputs)}, {"descriptions", *p.descriptions} };
}

void from_json(const json& j, ICDesc& p) {
    j.at("inputs").get_to(*(p.inputs));
    j.at("outputs").get_to(*(p.outputs));
    j.at("descriptions").get_to(*(p.descriptions));
}