#include "integrated/ic_desc.hpp"
#include "drawables/drawable_gate.hpp"
#include "drawables/drawable_wire.hpp"
#include "drawables/drawable_lamp.hpp"
#include "drawables/drawable_switch.hpp"
#include <vector>

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

        if (dynamic_cast<DrawableGate*>(dc) != NULL) { // It is a drawablegate
            DrawableGate* gate = dynamic_cast<DrawableGate*>(dc);
            type = gate->logic->GetLogicName();
        }
        if (dynamic_cast<DrawableSwitch*>(dc) != NULL) {
            type = "Switch";
        }
        if (dynamic_cast<DrawableLamp*>(dc) != NULL) {
            type = "Lamp";
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

                    ICConnectionDesc iccd = { indexOf, wire->fromIndex, wire->toIndex };
                    to.push_back(iccd);
                }
            }
        }

        ICComponentDesc iccc = { type, "", to };

        vicc->push_back(iccc);
    }
    return vicc;
}