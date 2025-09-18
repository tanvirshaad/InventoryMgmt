# -*- coding: utf-8 -*-

from odoo import api, fields, models, _
import json

class InventoryConnectorFieldAggregation(models.Model):
    _name = 'inventory.connector.field.aggregation'
    _description = 'Inventory Field Aggregation'

    field_name = fields.Char('Field Name', required=True)
    field_type = fields.Selection([
        ('text', 'Text'),
        ('multiline', 'Multiline Text'),
        ('numeric', 'Numeric'),
        ('boolean', 'Boolean'),
        ('document', 'Document')
    ], string='Field Type', required=True, default='text')
    inventory_id = fields.Many2one('inventory.connector.inventory', string='Inventory', required=True, ondelete='cascade')
    
    # Numeric field aggregations
    min_value = fields.Float('Minimum Value', digits=(16, 6))
    max_value = fields.Float('Maximum Value', digits=(16, 6))
    average_value = fields.Float('Average Value', digits=(16, 6))
    median_value = fields.Float('Median Value', digits=(16, 6))
    
    # Boolean field aggregations
    true_count = fields.Integer('True Count')
    false_count = fields.Integer('False Count')
    true_percentage = fields.Float('True Percentage', digits=(5, 2))
    
    # Text field aggregations (stored as JSON)
    common_values_json = fields.Text('Common Values (JSON)')
    
    # Computed fields for UI display
    display_aggregation = fields.Html('Aggregated Results', compute='_compute_display_aggregation')
    
    @api.depends('field_type', 'min_value', 'max_value', 'average_value', 'median_value',
                'true_count', 'false_count', 'true_percentage', 'common_values_json')
    def _compute_display_aggregation(self):
        """Compute a readable display of the aggregation"""
        for record in self:
            if record.field_type == 'numeric':
                record.display_aggregation = f"""
                    <strong>Min:</strong> {record.min_value}<br/>
                    <strong>Max:</strong> {record.max_value}<br/>
                    <strong>Average:</strong> {record.average_value:.2f}<br/>
                    <strong>Median:</strong> {record.median_value:.2f}
                """
            elif record.field_type == 'boolean':
                record.display_aggregation = f"""
                    <strong>True:</strong> {record.true_count} ({record.true_percentage:.1f}%)<br/>
                    <strong>False:</strong> {record.false_count} ({100 - record.true_percentage:.1f}%)<br/>
                """
            elif record.field_type in ['text', 'multiline'] and record.common_values_json:
                try:
                    common_values = json.loads(record.common_values_json)
                    html = "<strong>Most common values:</strong><br/><ul>"
                    for value in common_values:
                        html += f"""<li>"{value.get('value')}" - {value.get('frequency')} times ({value.get('percentage'):.1f}%)</li>"""
                    html += "</ul>"
                    record.display_aggregation = html
                except Exception as e:
                    record.display_aggregation = f"<em>Error parsing values: {str(e)}</em>"
            else:
                record.display_aggregation = "<em>No aggregation data available</em>"