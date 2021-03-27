#pragma once
#include "circuits/circuit_wire.hpp"
#include "circuits/circuit_io.hpp"
#include <vector>
#include <algorithm>

class CircuitOutput : public CircuitIO {
private:
    std::vector<CircuitWire*> signals;

public:
    CircuitOutput(LogicValue initialValue) : CircuitIO(initialValue) { signals = {}; }
    void AddOutputSignal(CircuitWire* wire) { signals.push_back(wire); }
    void RemoveOutputSignal(int index) { signals.erase(signals.begin() + index); }
    void SetSignals() {
        for (int i = 0; i < signals.size(); i++) {
            signals[i]->SetValue(this->GetValue());
        }
    }
};