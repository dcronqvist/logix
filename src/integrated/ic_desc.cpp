#include "integrated/ic_desc.hpp"
#include "drawables/drawable_gate.hpp"
#include "drawables/drawable_wire.hpp"
#include "drawables/drawable_lamp.hpp"
#include "drawables/drawable_switch.hpp"
#include "minimals/minimal_gate.hpp"
#include "minimals/minimal_switch.hpp"
#include "minimals/minimal_lamp.hpp"
#include "gate-logic/and_gate_logic.hpp"
#include "integrated/ic_input.hpp"
#include "integrated/ic_output.hpp"
#include "drawables/drawable_ic.hpp"
#include "minimals/minimal_ic.hpp"
#include <vector>
#include <string>

ICDesc::ICDesc(std::string name) {
    this->descriptions = new std::vector<ICComponentDesc>();
    this->inputs = {};
    this->outputs = {};
    this->name = name;
}

ICDesc::ICDesc(std::string name, std::vector<DrawableComponent*> components, std::vector<std::vector<std::string>> inps, std::vector<std::vector<std::string>> outs) {
    this->descriptions = GenerateDescriptions(components);
    this->inputs = inps;
    this->outputs = outs;
    this->name = name;
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
        ICDesc* icdesc = NULL;

        const char* type = "";
        const char* id = "";

        if (dynamic_cast<DrawableGate*>(dc) != NULL) { // It is a drawablegate
            DrawableGate* gate = dynamic_cast<DrawableGate*>(dc);
            type = gate->logic->GetLogicName();
        }
        if (dynamic_cast<DrawableSwitch*>(dc) != NULL) {
            type = "Switch";
            id = dynamic_cast<DrawableSwitch*>(dc)->id->c_str();
        }
        if (dynamic_cast<DrawableLamp*>(dc) != NULL) {
            type = "Lamp";
            id = dynamic_cast<DrawableLamp*>(dc)->id->c_str();
        }
        if (dynamic_cast<DrawableIC*>(dc) != NULL) {
            type = "IC";
            icdesc = &(dynamic_cast<DrawableIC*>(dc)->description);
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

        if (icdesc != NULL) {
            iccc.desc = icdesc;
        }

        vicc->push_back(iccc);
    }
    return vicc;
}

CircuitComponent* FindIOByID(std::vector<CircuitComponent*> comps, std::string id) {
    for (int i = 0; i < comps.size(); i++) {
        MinimalSwitch* ms = dynamic_cast<MinimalSwitch*>(comps.at(i));
        if (ms != NULL) {
            if (ms->id == id) { return comps.at(i); }
        }

        MinimalLamp* ml = dynamic_cast<MinimalLamp*>(comps.at(i));
        if (ml != NULL) {
            if (ml->id == id) { return comps.at(i); }
        }
    }
    return NULL;
}

std::vector<CircuitComponent*> ICDesc::GenerateComponents() {
    std::vector<CircuitComponent*> comps = {};

    for (int i = 0; i < this->descriptions->size(); i++) {
        ICComponentDesc compDesc = this->descriptions->at(i);
        CircuitComponent* comp;

        if (compDesc.type != "Switch" && compDesc.type != "Lamp" && compDesc.type != "IC") {
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
        else if (compDesc.type == "IC") {
            comp = new MinimalIC(*(compDesc.desc));
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

std::vector<CircuitInput*> ICDesc::GenerateICInputs(std::vector<CircuitComponent*> comps) {
    std::vector<CircuitInput*> icinputs = {};

    for (int inp = 0; inp < this->inputs.size(); inp++) {
        std::vector<MinimalSwitch*> switchMap = {};

        for (int swit = 0; swit < this->inputs.at(inp).size(); swit++) {
            switchMap.push_back(dynamic_cast<MinimalSwitch*>(FindIOByID(comps, this->inputs.at(inp).at(swit))));
        }
        ICInput* ici;
        int bits = (int)(this->inputs.at(inp).size());
        if (bits > 1) {
            ici = new ICInput{ bits, switchMap, dynamic_cast<MinimalSwitch*>(FindIOByID(comps, this->inputs.at(inp).front()))->id + "-" + dynamic_cast<MinimalSwitch*>(FindIOByID(comps, this->inputs.at(inp).back()))->id };
        }
        else {
            ici = new ICInput{ bits, switchMap, dynamic_cast<MinimalSwitch*>(FindIOByID(comps, this->inputs.at(inp).front()))->id };
        }

        icinputs.push_back(ici);
    }

    return icinputs;
}

std::vector<CircuitOutput*> ICDesc::GenerateICOutputs(std::vector<CircuitComponent*> comps) {
    std::vector<CircuitOutput*> icinputs = {};

    for (int inp = 0; inp < this->outputs.size(); inp++) {
        std::vector<MinimalLamp*> switchMap = {};

        for (int swit = 0; swit < this->outputs.at(inp).size(); swit++) {
            switchMap.push_back(dynamic_cast<MinimalLamp*>(FindIOByID(comps, this->outputs.at(inp).at(swit))));
        }
        ICOutput* ici;
        int bits = (int)(this->outputs.at(inp).size());
        if (bits > 1) {
            ici = new ICOutput{ bits, switchMap, dynamic_cast<MinimalLamp*>(FindIOByID(comps, this->outputs.at(inp).front()))->id + "-" + dynamic_cast<MinimalLamp*>(FindIOByID(comps, this->outputs.at(inp).back()))->id };
        }
        else {
            ici = new ICOutput{ bits, switchMap, dynamic_cast<MinimalLamp*>(FindIOByID(comps, this->outputs.at(inp).front()))->id };
        }

        icinputs.push_back(ici);
    }

    return icinputs;
}

void ICDesc::SetAdditionalText(std::string text) {
    this->additionalText = text;
}

void to_json(json& j, const ICDesc& p) {
    j = json{ {"name", p.name}, {"inputs", p.inputs}, {"outputs", p.outputs}, {"descriptions", *p.descriptions}, { "additionalText", p.additionalText} };
}

void from_json(const json& j, ICDesc& p) {
    j.at("inputs").get_to(p.inputs);
    j.at("name").get_to(p.name);
    j.at("outputs").get_to(p.outputs);
    j.at("descriptions").get_to(*(p.descriptions));
    j.at("additionalText").get_to(p.additionalText);
}