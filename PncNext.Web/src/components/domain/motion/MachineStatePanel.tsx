import styled from 'styled-components';

const Panel = styled.div`
  background-color: var(--panel-bg);
  border-radius: 8px;
  padding: 20px;
  border: 1px solid var(--border-color);
  box-shadow: 0 2px 4px rgba(0,0,0,0.05);
`;

const PanelTitle = styled.div`
  font-size: 16px;
  font-weight: 700;
  margin-bottom: 15px;
  color: var(--primary-color);
  display: flex;
  align-items: center;
  gap: 8px;
  border-bottom: 1px solid var(--border-color);
  padding-bottom: 10px;
`;

const StateGrid = styled.div`
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 12px;
`;

const StateItem = styled.div`
  background: var(--item-bg);
  padding: 12px;
  border-radius: 6px;
  display: flex;
  flex-direction: column;
  gap: 4px;
  border: 1px solid #eee;
`;

const StateLabel = styled.span`
  font-size: 10px;
  color: var(--text-muted);
  text-transform: uppercase;
  font-weight: 600;
`;

const StateValue = styled.span<{ color?: string }>`
  font-size: 16px;
  font-weight: 700;
  font-family: 'Consolas', monospace;
  color: ${props => props.color || 'var(--primary-color)'};
`;

const LedPanel = styled.div`
  display: flex;
  flex-wrap: wrap;
  gap: 15px;
  margin-top: 20px;
  padding: 15px;
  background-color: var(--item-bg);
  border-radius: 6px;
  border: 1px solid #eee;
`;

const LedItem = styled.div`
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 12px;
`;

const Led = styled.div<{ active?: boolean; variant?: 'success' | 'warning' | 'error' }>`
  width: 12px;
  height: 12px;
  border-radius: 50%;
  background: ${props => {
    if (!props.active) return '#333';
    switch (props.variant) {
      case 'warning': return 'var(--warning-color)';
      case 'error': return 'var(--error-color)';
      default: return 'var(--success-color)';
    }
  }};
  box-shadow: ${props => props.active ? `0 0 8px ${
    props.variant === 'warning' ? 'var(--warning-color)' : 
    props.variant === 'error' ? 'var(--error-color)' : 'var(--success-color)'
  }` : 'none'};
`;

interface MachineStatePanelProps {
  state: {
    gplErrorCode: string;
    homeStatus: string;
    toolNo: string;
    toolLength: string;
    spindleRun: boolean;
    spindleRpm: number;
    feedrate: number;
    overrideS: number;
    overrideF: number;
    flags: {
      vogue: boolean;
      ready: boolean;
      busy: boolean;
      pause: boolean;
      alarm: boolean;
      spindleOk: boolean;
      airOk: boolean;
    };
  };
}

const MachineStatePanel = ({ state }: MachineStatePanelProps) => {
  return (
    <Panel>
      <PanelTitle>⚙️ Machine Detailed State</PanelTitle>
      <StateGrid>
        <StateItem>
          <StateLabel>GPLErrorCode</StateLabel>
          <StateValue color="var(--error-color)">{state.gplErrorCode}</StateValue>
        </StateItem>
        <StateItem>
          <StateLabel>Home Status</StateLabel>
          <StateValue>{state.homeStatus}</StateValue>
        </StateItem>
        <StateItem>
          <StateLabel>Tool No / Length</StateLabel>
          <StateValue>T{state.toolNo} / {state.toolLength}mm</StateValue>
        </StateItem>
        <StateItem>
          <StateLabel>Spindle Run / RPM</StateLabel>
          <StateValue>{state.spindleRun ? 'RUN' : 'STOP'} / {state.spindleRpm}</StateValue>
        </StateItem>
        <StateItem>
          <StateLabel>Motor Feedrate</StateLabel>
          <StateValue>{state.feedrate} mm/min</StateValue>
        </StateItem>
        <StateItem>
          <StateLabel>Override (S/F)</StateLabel>
          <StateValue>{state.overrideS}% / {state.overrideF}%</StateValue>
        </StateItem>
      </StateGrid>

      <LedPanel>
        <LedItem><Led active={state.flags.vogue} /> Vogue</LedItem>
        <LedItem><Led active={state.flags.ready} /> Ready</LedItem>
        <LedItem><Led active={state.flags.busy} /> Busy</LedItem>
        <LedItem><Led active={state.flags.pause} variant="warning" /> Pause</LedItem>
        <LedItem><Led active={state.flags.alarm} variant="error" /> Alarm</LedItem>
        <LedItem><Led active={state.flags.spindleOk} /> SpindleOK</LedItem>
        <LedItem><Led active={state.flags.airOk} /> AirOK</LedItem>
      </LedPanel>
    </Panel>
  );
};

export default MachineStatePanel;
