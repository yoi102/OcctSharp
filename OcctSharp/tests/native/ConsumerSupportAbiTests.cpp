#include "OcctSharp.ConsumerSketch.Generated.h"
#include "OcctSharp.ConsumerSerialization.Generated.h"
#include "OcctSharp.ConsumerGuidedPreparation.Generated.h"
#include "OcctSharp.ConsumerAuthoringReports.Generated.h"
#include "OcctSharp.DefeatureTopologyHistory.Generated.h"
#include "../Visualization/OcctSharp.ConsumerInitialDisplay.Generated.h"
#include "../Visualization/OcctSharp.ConsumerViewerSources.Generated.h"
#include <cstddef>
#include <type_traits>

// Independent native side of the managed interoperability layout contract.
static_assert(sizeof(OcctSharp_SketchCurve) == 104);
static_assert(sizeof(OcctSharp_SketchExpressionNode) == 120);
static_assert(offsetof(OcctSharp_SketchExpressionNode, curve) == 16);
static_assert(sizeof(OcctSharp_SketchNativeNode) == 120);
static_assert(offsetof(OcctSharp_SketchNativeNode, origin) == 72);
static_assert(sizeof(OcctSharp_SketchNativeSummary) == 32);
static_assert(sizeof(OcctSharp_SerializedReferenceInfo) == 32);
static_assert(sizeof(OcctSharp_SerializedReferenceRow) == 32);
static_assert(offsetof(OcctSharp_SerializedReferenceRow, reserved) == 28);
static_assert(sizeof(OcctSharp_SerializedOccurrenceInfo) == 40);
static_assert(sizeof(OcctSharp_SerializedOccurrenceRow) == 40);
static_assert(offsetof(OcctSharp_SerializedOccurrenceRow, native_path_token_equal) == 32);
static_assert(offsetof(OcctSharp_SerializedOccurrenceRow, reserved) == 36);
static_assert(std::is_standard_layout_v<OcctSharp_SketchExpressionNode>);
static_assert(std::is_standard_layout_v<OcctSharp_SerializedOccurrenceRow>);
static_assert(sizeof(OcctSharp_AuthoringCopyInfo) == 16);
static_assert(sizeof(OcctSharp_AuthoringCopyArgumentInfo) == 16);
static_assert(sizeof(OcctSharp_AuthoringCopyRowInfo) == 32);
static_assert(offsetof(OcctSharp_AuthoringCopyRowInfo, target_index) == 12);
static_assert(offsetof(OcctSharp_AuthoringCopyRowInfo, issues) == 28);
static_assert(sizeof(OcctSharp_AuthoringHistoryDomainInfo) == 16);
static_assert(sizeof(OcctSharp_AuthoringHistoryDomainRowInfo) == 48);
static_assert(offsetof(OcctSharp_AuthoringHistoryDomainRowInfo, target_final_index) == 32);
static_assert(offsetof(OcctSharp_AuthoringHistoryDomainRowInfo, reserved) == 44);
static_assert(std::is_standard_layout_v<OcctSharp_AuthoringHistoryDomainRowInfo>);
int main() { return 0; }
