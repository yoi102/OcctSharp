#include "Runtime/OcctSharp.Runtime.OwnedSupport.Generated.hxx"
#include <atomic>
#include <barrier>
#include <cassert>
#include <iostream>
#include <thread>
#include <unordered_set>

namespace {
std::atomic<int> Live{0};
struct Value {
  const Value* Address = this;
  const int Number;
  explicit Value(int number) : Number(number) { ++Live; }
  Value(const Value&) = delete;
  Value(Value&&) = delete;
  ~Value() { --Live; }
};
struct Other {};
struct Token;
template<class T> bool Rejected(const void* token) {
  try { (void)OcctSharpOwned::Lease<T>(token); }
  catch (const OcctSharpGenerated::GeneratedOperationFailure& error) {
    return error.Status == OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  return false;
}
}

int main() {
  assert(Rejected<Value>(nullptr));
  auto* token = OcctSharpOwned::Publish<Token>(std::make_shared<Value>(42));
  assert(Rejected<Other>(token));
  OcctSharpOwned::Release(token, typeid(Other));
  auto lease = OcctSharpOwned::Lease<Value>(token);
  assert(lease->Address == lease.get() && lease->Number == 42);
  OcctSharpOwned::Release(token, typeid(Value));
  OcctSharpOwned::Release(token, typeid(Value));
  assert(Rejected<Value>(token) && Live == 1);
  lease.reset();
  assert(Live == 0);
  std::unordered_set<const void*> seen{token};
  for (int iteration = 0; iteration < 250; ++iteration) {
    auto* current = OcctSharpOwned::Publish<Token>(std::make_shared<Value>(iteration));
    assert(seen.insert(current).second);
    std::barrier ready(2);
    std::thread caller([&] {
      auto retained = OcctSharpOwned::Lease<Value>(current);
      ready.arrive_and_wait();
      for (int call = 0; call < 64; ++call) {
        assert(retained->Address == retained.get() && retained->Number == iteration);
        try {
          auto acquired = OcctSharpOwned::Lease<Value>(current);
          assert(acquired->Address == acquired.get() && acquired->Number == iteration);
        } catch (const OcctSharpGenerated::GeneratedOperationFailure& error) {
          assert(error.Status == OCCTSHARP_STATUS_INVALID_ARGUMENT);
        }
      }
    });
    ready.arrive_and_wait();
    OcctSharpOwned::Release(current, typeid(Value));
    caller.join();
    assert(Rejected<Value>(current) && Live == 0);
  }
  std::cout << "Owned registry PASS: null, type, stale, double release, no token reuse, stable address, 250 release races / 16000 acquisitions, all destructors.\n";
}
